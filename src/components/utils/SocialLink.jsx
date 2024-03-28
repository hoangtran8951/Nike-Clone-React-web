

const SocialLink = (props) => {
    // eslint-disable-next-line react/prop-types
    const {icon} = props;
  return (
    <img 
        src={icon}
        alt="icon/social"
        className="w-8 h-8 flex items-center overflow-hidden cursor-pointer md:w-6 md:h-6 sm:w-5 sm:h-5 transition-all duration-200 hover:scale-110 opacity-100 z-10"
    />
  )
}

export default SocialLink